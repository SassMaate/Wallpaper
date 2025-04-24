import AOS from 'aos'
import 'aos/dist/aos.css'

export default defineNuxtPlugin((nuxtApp) => {
  if (typeof window !== 'undefined') {
    nuxtApp.hook('app:mounted', () => {
      AOS.init({
        duration: 800,
        easing: 'ease-out',
        once: true,
        offset: 120,
        delay: 100
      })
    })
  }
}) 