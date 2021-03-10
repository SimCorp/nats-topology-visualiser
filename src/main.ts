import Vue from 'vue'
import axios from 'axios'
Vue.config.productionTip = false

new Vue({
  el: '#app',
  data () {
    return {
      info: null
    }
  },
  mounted () {
    axios
      .get('https://localhost:5001/')
      .then(response => (console.log(response.data)))
  }
}).$mount('#app')
